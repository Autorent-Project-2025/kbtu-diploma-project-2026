/**
 * Fast-path intent classifier. Runs BEFORE the LLM parser to short-circuit
 * obvious cases without paying the 20s qwen2.5:1.5b latency.
 *
 * Buckets:
 *  - "greeting"   : pure greeting → skip LLM, return empty filters directly
 *  - "gibberish"  : no recognizable tokens → clarify
 *  - "search"     : contains brand/model/style/budget/year/rating signal → LLM parser
 *  - "ambiguous"  : short prompt with no signals → let LLM + clarification handle it
 */

import {
  STYLE_DICTIONARY,
  TRANSMISSION_DICTIONARY,
  getBrandDictionary,
  getAliasToCanonicalBrand,
  getAliasToCanonicalModel,
  getModelToBrandDictionary,
} from "../queryTaxonomy";

export type Intent = "greeting" | "gibberish" | "search" | "ambiguous";

const GREETING_PATTERNS = [
  /^(привет|здорово|здравствуй(?:те)?|добр(?:ое|ый|ого) (?:утро|день|вечер)|хай|хеллоу)[\s!?.,]*$/iu,
  /^(hi|hello|hey|good (?:morning|afternoon|evening|day))[\s!?.,]*$/iu,
  /^(salam|салам|ассалам)[\s!?.,]*$/iu,
];

const SMALLTALK_PATTERNS = [
  /^(как дела|как ты|что нового|how are you|what's up)[\s!?.,]*$/iu,
  /^(спасибо|благодарю|thanks|thank you)[\s!?.,]*$/iu,
];

const BUDGET_CUES = /\b(до\s*\d|не\s+дороже|не\s+больше|бюджет|цена|стоимост|цен[уе]|under\s*\d|max\s*\d|budget|cheap|дешёв|дешев|деш[её]в)\b/iu;
const YEAR_CUES = /\b(от|до|с|по)\s*(?:19|20)\d{2}|\b(?:19|20)\d{2}\s*(?:\+|года|year)|new(?:er)?\b/iu;
const PASSENGER_CUES = /\b(\d+\s*(?:мест|человек|passenger|seat|people))|\bдля\s+(?:двоих|троих|четверых)\b/iu;
const RATING_CUES = /\b(рейтинг|ratings?|звёзд|stars|оцен|popular)\b/iu;
const TENURE_CUES = /\b(на\s+(?:день|неделю|выходные|месяц|сутки|час))|for\s+a?\s*(?:day|weekend|week|month|hour)\b/iu;

export function classifyIntent(prompt: string): Intent {
  const raw = prompt.trim();
  if (!raw) return "gibberish";

  const normalized = raw.toLowerCase();

  // Greetings and small talk — short-circuit.
  if (GREETING_PATTERNS.some((p) => p.test(normalized))) return "greeting";
  if (SMALLTALK_PATTERNS.some((p) => p.test(normalized))) return "greeting";

  // Check against every known signal source.
  if (containsKnownBrandOrModel(normalized)) return "search";
  if (containsStyleCue(normalized)) return "search";
  if (containsTransmissionCue(normalized)) return "search";
  if (BUDGET_CUES.test(normalized)) return "search";
  if (YEAR_CUES.test(normalized)) return "search";
  if (PASSENGER_CUES.test(normalized)) return "search";
  if (RATING_CUES.test(normalized)) return "search";
  if (TENURE_CUES.test(normalized)) return "ambiguous";

  // Look for any alphabetic token at all — if nothing, it's gibberish.
  const alphabeticTokens = normalized.match(/[\p{L}]{3,}/gu) ?? [];
  if (alphabeticTokens.length === 0) return "gibberish";

  // Has words but no recognizable signal — could be a model we don't know,
  // or a vague request. Let LLM handle.
  return "ambiguous";
}

function containsKnownBrandOrModel(normalized: string): boolean {
  const brands = getBrandDictionary();
  const models = getModelToBrandDictionary();
  const aliasToBrand = getAliasToCanonicalBrand();
  const aliasToModel = getAliasToCanonicalModel();

  for (const brand of brands) {
    if (brand && normalized.includes(brand)) return true;
  }
  for (const model of Object.keys(models)) {
    if (model && normalized.includes(model)) return true;
  }
  for (const alias of Object.keys(aliasToBrand)) {
    if (alias && normalized.includes(alias)) return true;
  }
  for (const alias of Object.keys(aliasToModel)) {
    if (alias && normalized.includes(alias)) return true;
  }
  return false;
}

function containsStyleCue(normalized: string): boolean {
  for (const { variants } of STYLE_DICTIONARY) {
    if (variants.some((v) => normalized.includes(v.toLowerCase()))) return true;
  }
  return false;
}

function containsTransmissionCue(normalized: string): boolean {
  for (const { variants } of TRANSMISSION_DICTIONARY) {
    if (variants.some((v) => normalized.includes(v.toLowerCase()))) return true;
  }
  return false;
}
